#!/usr/bin/env python3

import os
import sys

def validate_script(script_path):
    """Validate the deployment script for basic syntax issues"""
    try:
        with open(script_path, 'r', encoding='utf-8') as f:
            lines = f.readlines()
        
        # Check for shebang
        if not lines[0].startswith('#!'):
            print("WARNING: Missing shebang line")
        
        # Check for basic structure
        content = ''.join(lines)
        required_sections = [
            'install_prerequisites',
            'create_env_file',
            'build_backend',
            'build_frontend',
            'initialize_docker_stack',
            'run_database_migrations'
        ]
        
        missing_sections = []
        for section in required_sections:
            if section not in content:
                missing_sections.append(section)
        
        if missing_sections:
            print(f"WARNING: Missing sections: {', '.join(missing_sections)}")
        else:
            print("SUCCESS: All required sections found")
            
        # Check for balanced quotes and brackets
        quote_count = content.count('"') + content.count("'")
        if quote_count % 2 != 0:
            print("WARNING: Unbalanced quotes detected")
            
        bracket_count = content.count('{') - content.count('}')
        if bracket_count != 0:
            print("WARNING: Unbalanced curly brackets detected")
            
        bracket_count = content.count('[') - content.count(']')
        if bracket_count != 0:
            print("WARNING: Unbalanced square brackets detected")
            
        bracket_count = content.count('(') - content.count(')')
        if bracket_count != 0:
            print("WARNING: Unbalanced parentheses detected")
            
        # Check for common syntax errors
        if 'function' in content and not any(keyword in content for keyword in ['()', '{']):
            print("WARNING: Possible function declaration syntax issues")
            
        print("Validation completed.")
        return True
        
    except Exception as e:
        print(f"ERROR: {str(e)}")
        return False

if __name__ == "__main__":
    script_path = "deploy.sh"
    if len(sys.argv) > 1:
        script_path = sys.argv[1]
        
    if os.path.exists(script_path):
        validate_script(script_path)
    else:
        print(f"ERROR: Script file {script_path} not found")
        sys.exit(1)